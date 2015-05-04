# MobileProject
The project for Mobile computation Course
Activity Recognition for Restless Leg Syndrome Tracking
Önder Kaya

This repository contains a file with title Form1.cs. This file is actually the code behind of a Windows Form Application.

Please follow the steps for including these codes to your Windows Form Project successfully.

-Create a Windows Form Application on .Net 4.5. Visual Studio Express is free to use, you can download for idle
-Form name must be Form1. It's the default name that is given by Visual Studio
-Create two list box forms. The names must be listBox1 and listBox2. These are also the default names
-Create a chart form with name chart1.
-Replace the code behind with the file in the repository

csv file name must be "ActFeaturesData_Onder_0f74a0.csv". It must be placed next to the executable file. The executable file appears under ./bin/debug after debugging. If you want to change file name, plase find the line below and modify:
StreamReader input = new StreamReader("ActFeaturesData_Onder_0f74a0.csv");
If you want to define a different directory to place the csv file, please find the line below, uncomment it and modify the directory:
//Directory.SetCurrentDirectory("C:\\Users\\önder\\Documents\\Master II\\Mobile\\Project");

Methods:
-private List<DataTable> PrepareInputs(): reads the csv file, groups the data according to dates and fills the list boxes
-private void PrepareChart(): prepare the chart in case index of one of the list boxes is changed.

Input csv file must be activity recognition output of the AR Service
