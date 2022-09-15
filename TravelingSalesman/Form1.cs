using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravelingSalesman
{
    public partial class Form1 : Form
    {
        private List<float[]> xyPoints = new List<float[]>();
        private float xMin;
        private float xMax;
        private float yMin;
        private float yMax;
        private Random rnd = new Random();

        int sizeOfPopulation = 100;
        int chanceToMutate = 0;
        int numberOfGenerations = 50000;
        int weightValueEquationCurveExponent = 2;

        List<int> reproductionWeights = new List<int>();
        List<int> deathWeights = new List<int>();
        int weightsSum = 0;

        TreeNode root = new TreeNode();


        public Form1(List<float[]> xyCoordinates)
        {
            this.xyPoints = xyCoordinates;
            xMin = GetMin(this.xyPoints, 0);
            yMin = GetMin(this.xyPoints, 1);
            xMax = GetMax(this.xyPoints, 0);
            yMax = GetMax(this.xyPoints, 1);
            InitializeComponent();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;

            //PopulationMember solution = BeginGeneticAlgorithm(xyPoints, numberOfGenerations);
            //DrawMap(graphics, solution);
            //Console.WriteLine("Distance: " + solution.TotalDistance);

            List<float[]> solution = BackTrack(xyPoints);
            Console.WriteLine("Initial Distance: " + GetPathDistance(xyPoints));
            Console.WriteLine("Distance: " + GetPathDistance(solution));

        }

        private List<float[]> BackTrack(List<float[]> xyPoints)
        {
            float bestPathDistance = GetPathDistance(xyPoints);
            List<TreeNode> allValues = new List<TreeNode>();
            for (int i = 0; i < xyPoints.Count; i++)
            {
                TreeNode node = new TreeNode();
                node.SelfIndex = i;
                node.Self = xyPoints.ElementAt(i);
                allValues.Add(node);
            }

            GetTreeChildNodes(root, allValues);
            
            return xyPoints;
        }

        private void GetTreeChildNodes(TreeNode node, List<TreeNode> searchArea)
        {
            if (searchArea.Count != 0)
            {
                List<TreeNode> children = new List<TreeNode>();
                for (int i = 0; i < searchArea.Count; i++)
                {
                    TreeNode childNode = new TreeNode();
                    childNode.Parent = node.Self;
                    childNode.ParentIndex = node.SelfIndex;
                    childNode.Self = searchArea.ElementAt(i).Self;
                    childNode.SelfIndex = searchArea.ElementAt(i).SelfIndex;
                    children.Add(childNode);
                }
                node.Children.AddRange(children);

                for (int i = 0; i < node.Children.Count; i++)
                {
                    List<TreeNode> newSearchArea = new List<TreeNode>(searchArea);
                    TreeNode match = newSearchArea.FirstOrDefault(x => x.Self.SequenceEqual(children.ElementAt(i).Self));
                    newSearchArea.Remove(match);

                    GetTreeChildNodes(node.Children.ElementAt(i), newSearchArea);

                }

            }
        }

        private PopulationMember BeginGeneticAlgorithm(List<float[]> xyCoordinates, int numOfGenerations)
        {
            List<PopulationMember> population = new List<PopulationMember>(RandomInitialPopulation(xyCoordinates, sizeOfPopulation));
            population = population.OrderBy(x => x.TotalDistance).ToList();

            GetReproductionProbabilitiesForPopulationSize(population);
            AssignReproductionProbabilities(population);

            for (int i = 0; i < numOfGenerations; i++)
            {
                population.AddRange(BreedingFunction(population.ElementAt(WeightedSelectionOfPopulationMember(population, reproductionWeights)), population.ElementAt(WeightedSelectionOfPopulationMember(population, reproductionWeights)), chanceToMutate));
                population = population.OrderBy(x => x.TotalDistance).ToList();

                GetReproductionProbabilitiesForPopulationSize(population);
                AssignReproductionProbabilities(population);

                population.RemoveAt(WeightedSelectionOfPopulationMember(population, deathWeights));

                GetReproductionProbabilitiesForPopulationSize(population);
                AssignReproductionProbabilities(population);

                population.RemoveAt(WeightedSelectionOfPopulationMember(population, deathWeights));

                GetReproductionProbabilitiesForPopulationSize(population);
                AssignReproductionProbabilities(population);

                //Console.WriteLine("Shortest Path: " + population.ElementAt(0).TotalDistance + " Generation #: " + i);
            }
            return population.ElementAt(0);
        }

        private List<PopulationMember> BreedingFunction(PopulationMember parentOne, PopulationMember parentTwo, int mutationChance)
        {
            int parentOneFirstBreakpointIndex = rnd.Next(parentOne.Path.Count);
            int parentOneSecondBreakpointIndex = rnd.Next(parentOneFirstBreakpointIndex, parentOne.Path.Count);
            int parentTwoFirstBreakpointIndex = rnd.Next(parentTwo.Path.Count);
            int parentTwoSecondBreakpointIndex = rnd.Next(parentTwoFirstBreakpointIndex, parentTwo.Path.Count);

            List<float[]> parentOneClonedGenome = new List<float[]>(parentOne.Path.GetRange(parentOneFirstBreakpointIndex, parentOneSecondBreakpointIndex - parentOneFirstBreakpointIndex));
            List<float[]> parentTwoClonedGenome = new List<float[]>(parentTwo.Path.GetRange(parentTwoFirstBreakpointIndex, parentTwoSecondBreakpointIndex - parentTwoFirstBreakpointIndex));

            List<float[]> parentOneRemainder = new List<float[]>(parentOne.Path.Except(parentTwoClonedGenome).ToList());
            List<float[]> parentTwoRemainder = new List<float[]>(parentTwo.Path.Except(parentOneClonedGenome).ToList());

            int parentOneRemainderSpliceLocation = rnd.Next(parentOneRemainder.Count);
            int parentTwoRemainderSpliceLocation = rnd.Next(parentTwoRemainder.Count);

            parentOneRemainder.InsertRange(parentOneRemainderSpliceLocation, parentTwoClonedGenome);
            parentTwoRemainder.InsertRange(parentTwoRemainderSpliceLocation, parentOneClonedGenome);

            PopulationMember childOne = new PopulationMember();
            PopulationMember childTwo = new PopulationMember();
            childOne.Path = new List<float[]>(parentOneRemainder);
            childTwo.Path = new List<float[]>(parentTwoRemainder);

            if (mutationChance != 0)
            {
                childOne.Path = Mutate(childOne.Path, mutationChance);
                childTwo.Path = Mutate(childTwo.Path, mutationChance);
            }
            
            childOne.TotalDistance = GetPathDistance(childOne.Path);
            childTwo.TotalDistance = GetPathDistance(childTwo.Path);

            List<PopulationMember> newPopulationMembers = new List<PopulationMember>();
            newPopulationMembers.Add(childOne);
            newPopulationMembers.Add(childTwo);

            return newPopulationMembers;

        }

        private List<PopulationMember> RandomInitialPopulation(List<float[]> xyCoordinates, int populationSize)
        {
            List<PopulationMember> population = new List<PopulationMember>();
            for (int i = 0; i < populationSize; i++)
            {
                PopulationMember populationMember = new PopulationMember();
                populationMember.Path = ShuffleList(xyCoordinates);
                populationMember.TotalDistance = GetPathDistance(populationMember.Path);
                population.Add(populationMember);
            }
            return population;
        }

        private List<float[]> Mutate(List<float[]> path, int mutationChance)
        {
            int mutate = rnd.Next(mutationChance);
            if (mutate == 0)
            {
                int mutationLocationOne = rnd.Next(path.Count);
                int mutationLocationTwo = mutationLocationOne + 1 >= path.Count ? 0 : mutationLocationOne + 1;

                float[] tempOne = new float[2];
                tempOne[0] = path.ElementAt(mutationLocationOne)[0];
                tempOne[1] = path.ElementAt(mutationLocationOne)[1];
                path.ElementAt(mutationLocationOne)[0] = path.ElementAt(mutationLocationTwo)[0];
                path.ElementAt(mutationLocationOne)[1] = path.ElementAt(mutationLocationTwo)[1];
                path.ElementAt(mutationLocationTwo)[0] = tempOne[0];
                path.ElementAt(mutationLocationTwo)[1] = tempOne[1];
            }
            return path;
        }

        private List<float[]> ShuffleList(List<float[]> arrayToShuffle)
        {
            List<float[]> tempCollection = new List<float[]>(arrayToShuffle);
            List<float[]> shuffledXYCoordinatesArray = new List<float[]>();

            while (tempCollection.Count > 0)
            {
                int num = rnd.Next(tempCollection.Count);
                shuffledXYCoordinatesArray.Add(tempCollection.ElementAt(num));
                tempCollection.RemoveAt(num);
            }
            return shuffledXYCoordinatesArray;
        }

        private void AssignReproductionProbabilities(List<PopulationMember> population)
        {
            for (int i = 0; i < population.Count; i++)
            {
                population.ElementAt(i).ReproductionWeight = reproductionWeights.ElementAt(i);
                population.ElementAt(i).DeathWeight = deathWeights.ElementAt(i);
            }
        }

        private void GetReproductionProbabilitiesForPopulationSize(List<PopulationMember> population)
        {
            weightsSum = 0;
            reproductionWeights.Clear();
            deathWeights.Clear();
            for (int i = 0; i < population.Count; i++)
            {
                int weightValue = WeightValueEquation(population.Count, i, weightValueEquationCurveExponent);
                weightsSum = weightsSum + weightValue;
                reproductionWeights.Add(weightValue);
            }
            List<int> inverseProbabilities = new List<int>(reproductionWeights);
            inverseProbabilities.Reverse();
            deathWeights.AddRange(inverseProbabilities);


        }

        private int WeightedSelectionOfPopulationMember(List<PopulationMember> population, List<int> populationWeights)
        {
            int position = 0;
            int selection = rnd.Next(weightsSum);

            for (int i = 0; i < population.Count; i++)
            {
                position = position + populationWeights.ElementAt(i);
                if (position >= selection)
                {
                    return i;
                }
            }
            return populationWeights.ElementAt(populationWeights.Max());
        }

        private int WeightValueEquation(int sortedPopulationCount, int positionInSortedPopulation, int curveIntensity)
        {
            return (int)Math.Ceiling(((
                Math.Pow((sortedPopulationCount - positionInSortedPopulation), curveIntensity)
                / Math.Pow(sortedPopulationCount, curveIntensity - 1)) 
                /(sortedPopulationCount)) 
                * 100);
        }

        private void DrawMap(Graphics graphics, PopulationMember solution)
        {
            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black);
            graphics.Clear(Color.White);
            for (int a = 0; a < solution.Path.Count; a++)
            {
                graphics.FillEllipse(brush, XTranslate(solution.Path.ElementAt(a)[0]), YTranslate(solution.Path.ElementAt(a)[1]), 10, 10);
            }
            for (int j = 0; j < solution.Path.Count; j++)
            {
                if (j == solution.Path.Count - 1)
                {
                    graphics.DrawLine(pen,
                        XTranslate(solution.Path.ElementAt(j)[0]) + 5,
                        YTranslate(solution.Path.ElementAt(j)[1]) + 5,
                        XTranslate(solution.Path.ElementAt(0)[0]) + 5,
                        YTranslate(solution.Path.ElementAt(0)[1]) + 5);
                }
                else
                {
                    graphics.DrawLine(pen,
                        XTranslate(solution.Path.ElementAt(j)[0]) + 5,
                        YTranslate(solution.Path.ElementAt(j)[1]) + 5,
                        XTranslate(solution.Path.ElementAt(j + 1)[0]) + 5,
                        YTranslate(solution.Path.ElementAt(j + 1)[1]) + 5);
                }
            }
        }

        private float GetPathDistance(List<float[]> path)
        {

            float totalDistance = 0;

            for (int i = 0; i < path.Count; i++)
            {
                if (i == path.Count - 1)
                {
                    totalDistance = totalDistance + DistanceFormula(path.ElementAt(i), path.ElementAt(0));
                }
                else
                {
                    totalDistance = totalDistance + DistanceFormula(path.ElementAt(i), path.ElementAt(i + 1));
                }
            }

            return totalDistance;
        }

        private float DistanceFormula(float[] pointOne, float[] pointTwo)
        {
            return (float)Math.Sqrt(Math.Pow((pointTwo[0] - pointOne[0]), 2) + Math.Pow((pointTwo[1] - pointOne[1]), 2));
        }

        private float GetMin(List<float[]> xyCoordinates, int xy)
        {
            float min = 0;
            for (int i = 0; i < xyCoordinates.Count; i++)
            {
                if(i == 0)
                {
                    min = xyCoordinates.ElementAt(i)[xy];
                }
                else
                {
                    if (xyCoordinates.ElementAt(i)[xy] < min)
                    {
                        min = xyCoordinates.ElementAt(i)[xy];
                    }
                }
            }
            return min;
        }

        private float GetMax(List<float[]> xyCoordinates, int xy)
        {
            float max = 0;
            for (int i = 0; i < xyCoordinates.Count; i++)
            {
                if (i == 0)
                {
                    max = xyCoordinates.ElementAt(i)[xy];
                }
                else
                {
                    if (xyCoordinates.ElementAt(i)[xy] > max)
                    {
                        max = xyCoordinates.ElementAt(i)[xy];
                    }
                }
            }
            return max;
        }

        private float XTranslate(float xCoordinate)
        {
            float xRange = xMax - xMin;
            float xCoordinateDifferenceFromMin = xCoordinate - xMin;

            float xRatio = xCoordinateDifferenceFromMin / xRange;

            return (this.Width - this.Width / 10) * xRatio;
        }

        private float YTranslate(float yCoordinate)
        {
            float yRange = yMax - yMin;
            float yCoordinateDifferenceFromMax = yMax - yCoordinate;

            float yRatio = yCoordinateDifferenceFromMax / yRange;

            return (this.Height - this.Height / 10) * yRatio;
        }
    }
}
