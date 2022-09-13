﻿using System;
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

        int sizeOfPopulation = 25;
        int chanceToMutate = 0;
        int numberOfGenerations = 10000;
        int weightValueEquationCurveExponent = 2;

        List<int> reproductionProbabilities = new List<int>();
        int reproductionProbabilitiesSum = 0;


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
            BeginGeneticAlgorithm(e, xyPoints, numberOfGenerations);
        }

        private void BeginGeneticAlgorithm(PaintEventArgs e, List<float[]> xyCoordinates, int numOfGenerations)
        {
            Graphics graphics = e.Graphics;
            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black);

            List<PopulationMember> population = new List<PopulationMember>(RandomInitialPopulation(xyCoordinates, sizeOfPopulation));
            List<PopulationMember> sortedPopulation = population.OrderBy(x => x.TotalDistance).ToList();

            GetReproductionProbabilitiesForPopulationSize(sortedPopulation);
            AssignReproductionProbabilities(sortedPopulation);

            for (int i = 0; i < numOfGenerations; i++)
            {
                sortedPopulation.AddRange(BreedingFunction(GetMate(sortedPopulation), GetMate(sortedPopulation), chanceToMutate));
                sortedPopulation = sortedPopulation.OrderBy(x => x.TotalDistance).ToList();

                GetReproductionProbabilitiesForPopulationSize(sortedPopulation);
                AssignReproductionProbabilities(sortedPopulation);

                sortedPopulation.RemoveAt(sortedPopulation.Count - 1);
                sortedPopulation.RemoveAt(sortedPopulation.Count - 1);

                AssignReproductionProbabilities(sortedPopulation);

                float totalPopulationPathDistance = 0;
                for (int j = 0; j < sortedPopulation.Count; j++)
                {
                    totalPopulationPathDistance = totalPopulationPathDistance + sortedPopulation.ElementAt(j).TotalDistance;
                }

                Console.WriteLine("Shortest Path: " + sortedPopulation.ElementAt(0).TotalDistance + " Generation: " + i);
                //DrawMap(graphics, pen, brush, xyCoordinates, sortedPopulation);
            }
            DrawMap(graphics, pen, brush, xyCoordinates, sortedPopulation);
        }

        private PopulationMember GetMate(List<PopulationMember> sortedPopulation)
        {
            int mateSelection = rnd.Next(reproductionProbabilitiesSum);
            int matePosition = 0;
            for (int i = 0; i < reproductionProbabilities.Count; i++)
            {
                matePosition = matePosition + reproductionProbabilities.ElementAt(i);
                if (matePosition >= mateSelection)
                {
                    return sortedPopulation.ElementAt(i);
                }
            }
            return sortedPopulation.ElementAt(0);
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

        private void AssignReproductionProbabilities(List<PopulationMember> sortedPopulation)
        {
            for (int i = 0; i < sortedPopulation.Count; i++)
            {
                sortedPopulation.ElementAt(i).ReproductionProbability = reproductionProbabilities.ElementAt(i);
            }
        }

        private void GetReproductionProbabilitiesForPopulationSize(List<PopulationMember> sortedPopulation)
        {
            for (int i = 0; i < sortedPopulation.Count; i++)
            {
                int weightValue = WeightValueEquation(sortedPopulation.Count, i, weightValueEquationCurveExponent);
                reproductionProbabilitiesSum = reproductionProbabilitiesSum + weightValue;
                reproductionProbabilities.Add(weightValue);
            }
        }

        private int WeightValueEquation(int sortedPopulationCount, int positionInSortedPopulation, int curveIntensity)
        {
            return (int)Math.Ceiling(((
                Math.Pow((sortedPopulationCount - positionInSortedPopulation), curveIntensity)
                / Math.Pow(sortedPopulationCount, curveIntensity - 1)) 
                /(sortedPopulationCount)) 
                * 100);
        }

        private void DrawMap(Graphics graphics, Pen pen, Brush brush, List<float[]> xyCoordinates, List<PopulationMember> sortedPopulation)
        {
            graphics.Clear(Color.White);
            for (int a = 0; a < xyCoordinates.Count; a++)
            {
                graphics.FillEllipse(brush, XTranslate(xyPoints.ElementAt(a)[0]), YTranslate(xyPoints.ElementAt(a)[1]), 10, 10);
            }
            for (int j = 0; j < sortedPopulation.ElementAt(0).Path.Count; j++)
            {
                if (j == sortedPopulation.ElementAt(0).Path.Count - 1)
                {
                    graphics.DrawLine(pen,
                        XTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(j)[0]) + 5,
                        YTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(j)[1]) + 5,
                        XTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(0)[0]) + 5,
                        YTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(0)[1]) + 5);
                }
                else
                {
                    graphics.DrawLine(pen,
                        XTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(j)[0]) + 5,
                        YTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(j)[1]) + 5,
                        XTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(j + 1)[0]) + 5,
                        YTranslate(sortedPopulation.ElementAt(0).Path.ElementAt(j + 1)[1]) + 5);
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

            return this.Width * xRatio;
        }
        private float YTranslate(float yCoordinate)
        {
            float yRange = yMax - yMin;
            float yCoordinateDifferenceFromMax = yMax - yCoordinate;

            float yRatio = yCoordinateDifferenceFromMax / yRange;

            return this.Height * yRatio;
        }
    }
}
