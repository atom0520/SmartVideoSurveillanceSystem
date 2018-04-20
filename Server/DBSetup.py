import pymongo
from pymongo import MongoClient
import os
import configparser

config = configparser.ConfigParser()
config.read('..\config.ini')

DB_HOST = config['Database']['DB_HOST']
DB_PORT = int(config['Database']['DB_PORT'])

DB_NAME = 'SmartVideoSurveillanceSystem'
VIDEO_COLLECTION_NAME = 'Videos'

VIDEO_RELATIVE_DIRECTORY = r'..\Videos'
VIDEO_FULL_DIRECTORY = os.path.abspath(VIDEO_RELATIVE_DIRECTORY)

print(VIDEO_FULL_DIRECTORY)

client = MongoClient(DB_HOST, DB_PORT)
db = client[DB_NAME]

for root, dirs, files in os.walk(VIDEO_FULL_DIRECTORY):
    for filename in files:
        if filename.endswith(".avi"):
            print(filename)
            result = db[VIDEO_COLLECTION_NAME].find_one({'name':filename})
            if result!=None:
                print('Video \'{0}\' already exists with id as {1}'.format(filename, result['_id'])) 
                continue
            video = {
                'name': filename,
                'filePath': os.path.join(root, filename)
            }
            result = db[VIDEO_COLLECTION_NAME].insert_one(video)
            
            print('Added video \'{0}\' with id as {1}'.format(filename, result.inserted_id))
