# run with 50 instances
env="/home/bears_bears/Unity_projects/Flappy_bird/Builds/Linux_server/inference/Flappy_bird_RL_inference.x86_64"
#run_name=curriculum5_inference
config="inference.yaml"
solver_model="results/curriculum5/Solver/Solver-63252.pt"
gen_model="results/curriculum5/Generator/checkpoint.pt"

#resume previous training
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id run_name --no-graphics --env env --resume
#mlagents-learn ARLPCG_curriculum.yaml --num-envs 50 --run-id $run_name --env $env --resume --no-graphics 

#mlagents-learn ARLPCG_curriculum.yaml --run-id test_newGen --force # new generator

#mlagents-learn ARLPCG_curriculum_2.yaml --num-envs 40 --run-id $run_name --env $env --resume --no-graphics --time-scale 20


mlagents-learn $config --num-envs 20 --run-id inference --env $env --no-graphics --time-scale 5 --force --inference --initialize-from curriculum5
 
 

