@echo off
REM run with 50 instances
set env="Z:\Git\Flappy_bird\Builds\Windows\Inference\Flappy_bird.exe"
set run_name=InferCurr42d_0.5
set initRUN_ID=Curr42d
REM resume previous training
REM mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
REM mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 

REM mlagents-learn ARLPCG_curriculum.yaml --run-id test_newGen --force # new generator

REM mlagents-learn ARLPCG_curriculum_2.yaml --num-envs 40 --run-id $run_name --env $env --resume --no-graphics --time-scale 20

:: Execute this
::mlagents-learn ARLPCG_curriculum_6_GPU.yaml --num-envs=20 --run-id=%run_name% --env=%env% --no-graphics --torch-device=cuda --time-scale=5

mlagents-learn inference.yaml --num-envs=10 --run-id=%run_name% --env=%env%   --no-graphics --torch-device=cuda --time-scale=1 --inference --initialize-from=%initRUN_ID%


