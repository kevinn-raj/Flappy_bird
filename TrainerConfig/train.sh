# run with 50 instances
env="/home/bears_bears/Unity_projects/Flappy_bird/Builds/Linux_server/Curr30/Flappy_bird_RL_Curr30.x86_64"
run_name=curr30b
 


#resume previous training
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 

#mlagents-learn ARLPCG_curriculum.yaml --run-id test_newGen --force # new generator

#mlagents-learn ARLPCG_curriculum_2.yaml --num-envs 40 --run-id $run_name --env $env --resume --no-graphics --time-scale 20
mlagents-learn ARLPCG_curriculum_6_GPU.yaml --num-envs=40 --run-id=$run_name --env=$env --no-graphics  --torch-device=cuda --time-scale=3 



