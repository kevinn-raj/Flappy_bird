# -*- coding: utf-8 -*-
"""
Created on Sat May 13 12:14:29 2023

@author: Kevinn
"""

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

def solving_ratio(path):
    df = pd.read_csv(path, nrows=1000*7, header=None, names=["Score"])
    
    solved = (df[df>= 10]).count()
    total = df.count()
    ratio = solved / total
    print(float(ratio))
    print(float(total))
    return df
    
solving_ratio("ARL_1_score.csv")
solving_ratio("PCG_1_score.csv")
pd1 = solving_ratio("ARL_-1_score.csv")
pd2 = solving_ratio('PCG_-1_score.csv')
solving_ratio("ARL_-0.5_score.csv").hist()
