# run with 50 instances
env="/home/bears_bears/Unity_projects/Flappy_bird/Builds/Linux_server/3_curriculum/Flappy_bird_RL_curriculum_3.x86_64"
run_name=curriculum2
#resume previous training
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 



