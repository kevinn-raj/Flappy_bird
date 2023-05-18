# Flappy bird
## This repo is for creating and training a bird with Unity and ML-agents based on the original game
It contains a the Unity project folder we have built the model with.
- The codes for the analysis are stored in "Analysis/Codes/". 
  - __Curr__ means the auxiliary input $\lambda = 1, -1, 1, -1$ during training.
  - __NoCurr__ means the auxiliary input $\lambda$ is randomly sampled from $-1, -0.5, 0, 0.5, 1$ during training.
- The configurations a the runs of the results are located in "TrainerConfig/".
- The scripts for the gameplay and the models are in "Assets/Scripts/".
  - The agent scripts are __Solver.cs__, __Generator.cs__, __Solver_PCG.cs__
