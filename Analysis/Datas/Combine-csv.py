# -*- coding: utf-8 -*-
"""
Created on Sat May 13 11:54:26 2023

@author: Kevinn
"""

from glob import glob

with open('singleDataFile.csv', 'a') as singleFile:
    for csvFile in glob(r'Scores\*.csv'):
        for line in open(csvFile, 'r'):
            singleFile.write(line)