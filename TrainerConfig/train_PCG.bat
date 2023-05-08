@echo off
REM run with 50 instances
set env="Z:\Git\Flappy_bird\Builds\Windows\PCG_1\Flappy_bird.exe"
set run_name=PCG_1 
REM resume previous training
REM mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
REM mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 

REM mlagents-learn ARLPCG_curriculum.yaml --run-id test_newGen --force # new generator

REM mlagents-learn ARLPCG_curriculum_2.yaml --num-envs 40 --run-id $run_name --env $env --resume --no-graphics --time-scale 20

:: Execute this
mlagents-learn PCG.yaml  --num-envs=10 --run-id=%run_name%  --no-graphics --torch-device=cuda --time-scale=1  --resume --env=%env%

::mlagents-learn easy.yaml --num-envs=7 --run-id=%run_easy% --env=%env%   --no-graphics --torch-device=cuda --time-scale=3  --base-port=5000 
::
::mlagents-learn hard.yaml --num-envs=7 --run-id=%run_hard% --env=%env%   --no-graphics --torch-device=cuda --time-scale=3  --base-port=5000 --initialize-from=%run_easy%


