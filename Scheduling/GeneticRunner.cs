using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduling
{
    class GeneticMachine
    {
        int jobnum, procnum, macnum, krlength = 0;
        bool refresh;
        Schedule[] population;
        Random rnd = new Random();
        private Schedule best;
        double progress = 0;
        int produced = 0;
        bool stopped;
        int mutOdd;
        int groupSize;
        int minTimeOdd;
        int popSize;
        AsyncOperation aop = AsyncOperationManager.CreateOperation(null);
        COTypes crossOver;
        MutationTypes mutationTypes;
        SelectionTypes selectionType;
        public event EventHandler BestValueChanged;
        public event EventHandler ProgressChanged;
        int nothingFound;
        
        public GeneticMachine(int jobs, int procs, int macs, int popsize)
        {
            this.jobnum = jobs;
            this.procnum = procs;
            this.macnum = macs;
            krlength = jobs * procs;
            mutOdd = 1;
            minTimeOdd = 0;
            groupSize = 5;
            population = new Schedule[popsize];
            popSize = popsize;
            crossOver = COTypes.Uniform;
            mutationTypes = MutationTypes.ExchangeValues;
            selectionType = SelectionTypes.Tournament;
            best = null;
            nothingFound = 0;
        }
        
        #region Initial Pop
        
        void CreateInitialPopulation()
        {
            for (int i = 0; i < population.Length; i++)
            {
                Schedule ciz = createNewSchedule();
                population[i] = ciz;
                produced++;
                
                Progress = double.Parse((produced * 100d / 10000000).ToString("0.00"));
                if (stopped)
                    break;
            }
        }
        #endregion

        #region Start
        public void Start()
        {
            CreateInitialPopulation();
            Procedure();
        }
        #endregion

        void Procedure()
        {
            Task t1 = new Task(() =>
            {
                stopped = false;
                while (produced < 10000000)
                {
                    int[] nums = doSelection();

                    Schedule mother = population[nums[0]];
                    Schedule father = population[nums[1]];

                    Schedule child1 = doCrossover(mother, father);
                    Schedule child2 = doCrossover(father, mother);

                    doMutation(child1);
                    doMutation(child2);

                    population[nums[nums.Length - 1]] = child1;
                    population[nums[nums.Length - 2]] = child2;
                    checkBestValueChanged(child1);
                    checkBestValueChanged(child2);
                    produced++;
                    nothingFound++;
                    if (nothingFound > 300000 && refresh)
                    {
                        addNewChromosomes(popSize / 10);
                        nothingFound = 0;
                    }
                    Progress = double.Parse((produced * 100d / 10000000).ToString("0.00"));
                    if (stopped)
                        break;
                }
            });
            t1.Start();
        }
        void addNewChromosomes(int n)
        {
            int[] nums = new int[n];
            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = rnd.Next(0, population.Length);
                Schedule created = createNewSchedule();
                population[nums[nums.Length - 1 - i]] = created;
                checkBestValueChanged(created);
                produced++;
            }
            
        }
        Schedule createNewSchedule()
        {
            int[] jobSchedule = new int[krlength];
            int[] macSchedule = new int[krlength];
            int[] used = new int[jobnum];
            bool[] assigned = new bool[krlength];
            List<int> rem = Enumerable.Range(0, jobnum).ToList();
            for (int j = 0; j < krlength; j++)
            {
                int selJob = rem[rnd.Next(0, rem.Count)];
                jobSchedule[j] = selJob;
                if (rnd.Next(1, 101) <= minTimeOdd) //En küçük zamanı alarak popülasyonu iyileştir
                {
                    int minmac = Data.GetMinTimeMacForJP(selJob, used[selJob]);
                    int pos = selJob * procnum + used[selJob];
                    macSchedule[pos] = minmac;
                    assigned[pos] = true;
                }
                else
                {
                    if (!assigned[j])
                        macSchedule[j] = rnd.Next(0, macnum);
                }
                used[selJob]++;
                if (used[selJob] == procnum)
                    rem.Remove(selJob);
            }

           return new Schedule(jobSchedule, macSchedule, jobnum, procnum, macnum);
        }

        //***************************************************//
        
        #region DoSelection

        int[] doSelection()
        {
            int[] nums = null;
            switch (selectionType)
            {
                case SelectionTypes.Tournament:
                    nums = tourSelection(groupSize);
                    break;
                case SelectionTypes.RouletteWheel:
                    nums = rouletWheelSelection(groupSize);
                    break;
            }
            return nums;
        }

        #endregion
        
        
        #region DoMutation
        public void doMutation(Schedule child)
        {
            MutationTypes temp = mutationTypes;
            if(rnd.Next(1, 101) <= mutOdd)
            switch (temp)
            {
                case MutationTypes.ExchangeValues:
                    ExchangeValuesMutation(child);
                    break;
                case MutationTypes.ChangeValue:
                    ChangeValueMutation(child);
                    break;
                case MutationTypes.SlipBlock:
                    SlipBlockMutation(child);
                    break;
                case Scheduling.MutationTypes.Replacement:
                    ReplacementMutation(child);
                    break;
            }
        }
        #endregion
        
        
        #region DoCrossover
        
        Schedule doCrossover(Schedule mother, Schedule father)
        {
            COTypes temp = crossOver;
            Schedule child = null;
            switch (temp)
            {
                case COTypes.SinglePoint:
                    child = SinglePointCO(mother, father);
                    break;
                case COTypes.TwoPoint:
                    child = TwoPointCO(mother, father);
                    break;
                case COTypes.Uniform:
                    child = UniformCO(mother, father);
                    child.Repair();
                    break;
                case COTypes.OrderedX:
                    child = OrderedCO(mother, father);
                    break;
            }
            return child;
        }
        #endregion

        
        //***************************************************//


        #region Stop

        public void Stop()
        {
            stopped = true;
        }
        #endregion


        //***************************************************//


        #region Event Firings

        void checkBestValueChanged(Schedule tour)
        {
            if (best == null || best.FitValue > tour.FitValue)
            {
                best = tour;
                aop.Post(new System.Threading.SendOrPostCallback(delegate
                {
                    if (BestValueChanged != null)
                        BestValueChanged(this, EventArgs.Empty);
                }), null);
                nothingFound = 0;
            }
        }
        void fireProgressEvent()
        {
            aop.Post(new System.Threading.SendOrPostCallback(delegate
            {
                if (ProgressChanged != null)
                    ProgressChanged(this, EventArgs.Empty);
            }), null);
        }
        #endregion


        //***************************************************//
        

        #region --------Selection Methods---------

        #region Tour Selection

        int[] tourSelection(int groupSize)
        {
            int[] nums = new int[groupSize];

            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = rnd.Next(population.Length - 1);
            }
            sortArray(nums);
            return nums;
        }
        void sortArray(int[] nums)
        {
            bool sorted = true;
            while (sorted)
            {
                sorted = false;

                for (int i = 0; i < nums.Length - 1; i++)
                {
                    if (population[nums[i]].FitValue > population[nums[i + 1]].FitValue)
                    {
                        int temp = nums[i];
                        nums[i] = nums[i + 1];
                        nums[i + 1] = temp;
                        sorted = true;
                    }
                }
            }
        }
        #endregion

        #region Roulette Wheel Selection

        int[] rouletWheelSelection(int groupSize)
        {
            int[] nums = new int[groupSize];
            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = rnd.Next(population.Length - 1);
            }
            sortArray(nums);
            double[] reciprocal = new double[groupSize];
            for (int i = 0; i < reciprocal.Length; i++)
            {
                reciprocal[i] = 1 / population[nums[i]].FitValue;
            }
            for (int j = 0; j < 2; j++)
            {
                double value = rnd.NextDouble();

                for (int i = 0; i < reciprocal.Length; i++)
                {
                    value -= reciprocal[i];
                    if (value <= 0)
                    {
                        nums[j] = i;
                        break;
                    }
                }
            }

            return nums;
        }
        #endregion

        #endregion


        //***************************************************//

        #region --------Crossing Overs--------

        #region Uniform CO

        Schedule UniformCO(Schedule mother, Schedule father)
        {
            int N = krlength;
            int[] macs = mother.MacSchedule.Clone() as int[];
            int[] jobs = mother.JobSchedule.Clone() as int[];
            for (int i = 0; i < N; i++)
            {
                if (rnd.Next(0, 2) == 0)
                {
                    macs[i] = father.MacSchedule[i];
                }
                if (rnd.Next(0, 2) == 0)
                {
                    jobs[i] = father.JobSchedule[i];
                }
            }
            return new Schedule(jobs, macs, jobnum, procnum, macnum);
        }
        #endregion

        #region Two Point CO
        public Schedule TwoPointCO(Schedule mother, Schedule father)
        {
            int N = krlength;
            int elNum = rnd.Next(N / 4, 3 * N / 4);
            int x = rnd.Next(0, N - 1);
            int y = x + elNum - 1;
            int tt = rnd.Next(0, N - 1);
            int[] gens = new int[N];
            for (int i = 0; i < N; i++)
                gens[i] = -1;
            int[] used = new int[jobnum];
            for (int i = x; i <= y; i++)
            {
                gens[i % N] = father.JobSchedule[tt % N];
                used[gens[i % N]]++;
                tt++;
            }

            int k = 0;
            for (int i = 0; i < N; i++)
            {
                if (gens[i] != -1) continue;

                while (used[mother.JobSchedule[k]] >= procnum)
                    k++;
                gens[i] = mother.JobSchedule[k];
                used[gens[i]]++;
            }

            int[] macAS = mother.MacSchedule.Clone() as int[];
            elNum = rnd.Next(N / 4, 2 * N / 4);
            x = rnd.Next(0, N);
            tt = rnd.Next(0, N);
            int bound = x + elNum;
            while (x < bound)
            {
                macAS[x % N] = father.MacSchedule[tt % N];
                tt++;
                x++;
            }
            return new Schedule(gens, macAS, jobnum, procnum, macnum);
        }
        #endregion
        
        #region Single Point
        public Schedule SinglePointCO(Schedule mother, Schedule father)
        {
            int N = krlength;
            int x = rnd.Next(N / 4, 3 * N / 4);

            int[] gens = new int[N];
            for (int i = 0; i < N; i++)
                gens[i] = -1;

            int[] used = new int[jobnum];
            for (int i = 0; i < x; i++)
            {
                gens[i] = mother.JobSchedule[i];
                used[gens[i]]++;
            }

            int k = 0;
            for (int i = x; i < N; i++)
            {
                if (gens[i] != -1) continue;

                while (used[father.JobSchedule[k]] >= procnum)
                    k++;
                gens[i] = father.JobSchedule[k];
                used[gens[i]]++;
            }

            int[] macAS = new int[N];
            x = rnd.Next(N / 4, 3 * N / 4);
            for (int i = 0; i < x; i++)
            {
                macAS[i] = mother.MacSchedule[i];
            }
            for (int i = x; i < N; i++)
            {
                macAS[i] = father.MacSchedule[i];
            }
            return new Schedule(gens, macAS, jobnum, procnum, macnum);
        }
        #endregion

        #region Ordered CO

        Schedule OrderedCO(Schedule mother, Schedule father)
        {
            int N = krlength;
            //Job sequence CO
            int[] jobOS = new int[N];
            List<int> posOS = Enumerable.Range(0, N).ToList();
            List<int> fatRem = new List<int>();
            int el = rnd.Next(N / 4, 2 * N / 4);
            int x = rnd.Next(0, N - el);
            int y = (x + el - 1);
            for (int i = y + 1; i < N; i++)
            {
                fatRem.Add(father.JobSchedule[i]);
            }
            for (int i = 0; i <= y; i++)
            {
                fatRem.Add(father.JobSchedule[i]);
            }

            for (int i = x; i <= y; i++)
            {
                fatRem.Remove(mother.JobSchedule[i]);
            }
            int a = 0;
            for (int i = 0; i < N; i++)
            {
                if (i < x || i > y)
                {
                    jobOS[i] = fatRem[a];
                    a++;
                }
                else
                {
                    jobOS[i] = mother.JobSchedule[i];
                }
            }
            int[] macAS = new int[N];
            for (int j = 0; j < procnum; j++)
            {
                x = rnd.Next(N / 3, 2 * N / 3);
                for (int i = 0; i < x; i++)
                {
                    macAS[i] = mother.MacSchedule[i];
                }
                for (int i = x; i < N; i++)
                {
                    macAS[i] = father.MacSchedule[i];
                }
            }
            return new Schedule(jobOS, macAS, jobnum, procnum, macnum);
        }
        #endregion

        #endregion

        
        //***************************************************//


        #region --------Mutation Methods---------
        
        #region Change Value Mutation
        
        void ChangeValueMutation(Schedule sch)
        {
            int N = krlength;
            int x1 = rnd.Next(0, N);
            int mac = rnd.Next(0, macnum);
            sch.MacSchedule[x1] = mac;
        }
        #endregion

        #region Exchange Values Mutation

        void ExchangeValuesMutation(Schedule sch)
        {
            int N = krlength;
            int x1 = rnd.Next(0, N);
            int x2 = rnd.Next(0, N);

            int temp = sch.JobSchedule[x1];
            sch.JobSchedule[x1] = sch.JobSchedule[x2];
            sch.JobSchedule[x2] = temp;

            x1 = rnd.Next(0, krlength);
            x2 = rnd.Next(0, krlength);

            int macTemp = sch.MacSchedule[x1];
            sch.MacSchedule[x1] = sch.MacSchedule[x2];
            sch.MacSchedule[x2] = macTemp;
        }
        #endregion

        #region Slip Block Mutation

        void SlipBlockMutation(Schedule sch)
        {
            //Job part
            int N = krlength;
            int el = rnd.Next(1, N);
            int x1 = rnd.Next(0, N - el);
            int x2 = rnd.Next(0, N - el);
            x1 = Math.Min(x1, x2);
            x2 = Math.Max(x1, x2);
            int[] temps = new int[el];
            int kk = x1;
            for (int i = 0; i < temps.Length; i++)
            {
                temps[i] = sch.JobSchedule[kk];
                kk++;
            }
            int step = x2 - x1;
            int[] temps2 = new int[step];
            for (int i = x1 + el; i < x1 + el + step; i++)
            {
                temps2[i - x1 - el] = sch.JobSchedule[i];
            }
            for (int i = x1; i < x1 + temps2.Length; i++)
            {
                sch.JobSchedule[i] = temps2[i - x1];
            }
            for (int i = x2; i < x2 + temps.Length; i++)
            {
                sch.JobSchedule[i] = temps[i - x2];
            }

            //Machine part
            el = rnd.Next(1, N);
            x1 = rnd.Next(0, N - el);
            x2 = rnd.Next(0, N - el);
            x1 = Math.Min(x1, x2);
            x2 = Math.Max(x1, x2);
            int[] tempMacs = new int[el];
            kk = x1;
            for (int i = 0; i < tempMacs.Length; i++)
            {
                tempMacs[i] = sch.MacSchedule[kk];
                kk++;
            }
            step = x2 - x1;
            int[] tempMacs2 = new int[step];
            for (int i = x1 + el; i < x1 + el + step; i++)
            {
                tempMacs2[i - x1 - el] = sch.MacSchedule[i];
            }
            for (int i = x1; i < x1 + tempMacs2.Length; i++)
            {
                sch.MacSchedule[i] = tempMacs2[i - x1];
            }
            for (int i = x2; i < x2 + tempMacs.Length; i++)
            {
                sch.MacSchedule[i] = tempMacs[i - x2];
            }
        }
        #endregion

        #region Replacement Mutation

        void ReplacementMutation(Schedule sch)
        {
            int N = krlength;
            int x1 = rnd.Next(0, N);
            int x2 = rnd.Next(0, N);
            x1 = Math.Min(x1, x2);
            x2 = Math.Max(x1, x2);
            int selected = sch.JobSchedule[x1];
            for (int i = 0; i < sch.JobSchedule.Length; i++)
            {
                if (i > x1 && i <= x2)
                {
                    sch.JobSchedule[i - 1] = sch.JobSchedule[i];
                }
            }
            sch.JobSchedule[x2] = selected;
            x1 = rnd.Next(0, N);
            x2 = rnd.Next(0, N);

            int mac = sch.MacSchedule[x1];
            for (int i = 0; i < sch.JobSchedule.Length; i++)
            {
                if (i > x1 && i <= x2)
                {
                    sch.MacSchedule[i - 1] = sch.MacSchedule[i];
                }
            }
            sch.MacSchedule[x2] = mac;
        }
        #endregion

        #endregion


        //***************************************************//


        #region Properties
        public bool Refresh
        {
            get { return refresh; }
            set { refresh = value; }
        }
        public SelectionTypes SelectionType
        {
            get { return selectionType; }
            set { selectionType = value; }
        }
        
        public int MinTimeOdd
        {
            get { return minTimeOdd; }
            set { minTimeOdd = value; }
        }
        
        public int GroupSize
        {
            get { return groupSize; }
            set { groupSize = value; }
        }
        
        public int MutOdd
        {
            get { return mutOdd; }
            set { mutOdd = value; }
        }
        
        public MutationTypes MutationTypes
        {
            get { return mutationTypes; }
            set { mutationTypes = value; }
        }
        
        public COTypes CrossOver
        {
            get { return crossOver; }
            set { crossOver = value; }
        }
        
        public bool Stopped
        {
            get
            {
                return stopped;
            }
        }
        public Schedule Best
        {
            get { return best; }
            set { best = value; }
        }
        public double Progress
        {
            get
            {
                return progress;
            }
            set
            {
                if (progress != value)
                    fireProgressEvent();
                progress = value;
            }
        }

        #endregion

        void refreshPopulation()
        {
            for (int i = 0; i < popSize; i++)
            {
                Schedule ciz = createNewSchedule();
                population[i] = ciz;
                checkBestValueChanged(ciz);
            }
        }
    }
    
    public static class Data
    {
        public static float[, ,] DataTable;
        public static int GetMinTimeMacForJP(int job, int proc)
        {
            int result = -1;
            float min = -1;
            for (int i = 0; i < DataTable.GetLength(2) - 1; i++)
            {
                float curr = Math.Min(min == -1 ? Data.DataTable[job, proc, i] : min, Data.DataTable[job, proc, i + 1]);
                if (min == -1)
                {
                    result = (curr == DataTable[job, proc, i] ? i : i + 1);
                    min = curr;
                }
                if (curr < min)
                {
                    result = i + 1;
                    min = curr;
                }
            }
            return result;
        }
    }
    public enum COTypes
    {
        SinglePoint = 0,
        TwoPoint = 1,
        Uniform = 2,
        OrderedX = 3
    }
    public enum MutationTypes
    {
        ExchangeValues = 0,
        ChangeValue = 1,
        SlipBlock = 2,
        Replacement = 3
    }
    public enum SelectionTypes
    {
        Tournament = 0,
        RouletteWheel = 1
    }
}
