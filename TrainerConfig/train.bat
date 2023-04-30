REM run with 50 instances
set env="Z:\Git\Flappy_bird\Builds\Windows\Curr6\Flappy_bird.exe"
set run_name=curr6_GPU

REM resume previous training
REM mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
REM mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 

REM mlagents-learn ARLPCG_curriculum.yaml --run-id test_newGen --force # new generator

REM mlagents-learn ARLPCG_curriculum_2.yaml --num-envs 40 --run-id $run_name --env $env --resume --no-graphics --time-scale 20
mlagents-learn ARLPCG_curriculum_6_GPU.yaml --num-envs=10 --run-id=%run_name% --env=%env% --no-graphics --time-scale=1 --torch-device=cuda --resume



