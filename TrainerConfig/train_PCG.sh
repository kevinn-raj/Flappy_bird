# run with 50 instances
env="/home/bears_bears/Unity_projects/Flappy_bird/Builds/Linux_server/PCG_1/Flappy_bird_RL_PCG.x86_64"
run_name=PCG_1
 


#resume previous training
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 

#mlagents-learn ARLPCG_curriculum.yaml --run-id test_newGen --force # new generator

#mlagents-learn ARLPCG_curriculum_2.yaml --num-envs 40 --run-id $run_name --env $env --resume --no-graphics --time-scale 20
mlagents-learn PCG.yaml --num-envs=1 --run-id=$run_name --env $env --no-graphics  --torch-device=cuda --time-scale=1 --resume



