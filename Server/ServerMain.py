import socket
import cv2
import sys
import threading
import boto3
import json  
import pymongo
from pymongo import MongoClient
import configparser

config = configparser.ConfigParser()
config.read('..\config.ini')

SERVER_HOST = config['Server']['SERVER_HOST']
SERVER_PORT = int(config['Server']['SERVER_PORT'])

DB_HOST = config['Database']['DB_HOST']
DB_PORT = int(config['Database']['DB_PORT'])
DB_NAME = 'SmartVideoSurveillanceSystem'
VIDEO_COLLECTION_NAME = 'Videos'
DETECTION_COLLECTION_NAME = 'Detections'
DEVICE_COLLECTION_NAME = 'Devices'

PLATFORM_APPLICATION_NAME = 'SmartVideoSurveillanceSystem'
FCM_SERVER_API_KEY= "AAAAkX6pRkQ:APA91bGcHJQUFecxhcVKyWMKAW8UawFWmgxhANYXYKa4ETgLeRG2B0iJDGUa4jRMK9jbfsKA3s99sjtsFmjAXfFHwpvrHxuL1ql_LfU8WrHedL1p9jdGPqDvvmpd1T_lmWz7BSwiFsne"

COMMAND_ID_BYTE_NUM = 4

FRAME_SIZE_BYTE_NUM = 16
FRAME_NO_BYTE_NUM = 4

MESSAGE_LIST_LENGTH_BYTE_NUM = 16

DEFAULT_READ_BUFFER_SIZE = 64

def to_bytes(n, length, endianess='big'):
    h = '%x' % n
    s = ('0'*(len(h) % 2) + h).zfill(length*2).decode('hex')
    return s if endianess == 'big' else s[::-1]

def SendNotification(endpointArn):
    response = snsClient.publish(
        TargetArn=endpointArn,
        MessageStructure="json",
        Message=json.dumps({"default": "any value", 
        "GCM": '{ \"notification\": {\"title\": \"Fall Detected! 0 0\", \"body\": \"Someone fell down!\", \"sound\":\"default\" }, \"data\" : {\"alert\" : \"true\"} }' })
    )
    messageId = response['MessageId']
    print('Send MessageId: {0}'.format(messageId))

def SaveDetection(videoName, eventFrame, eventTime):
    detection = {
        'videoName': videoName,
        'eventFrame': eventFrame,
        'eventTime': eventTime
    }

    result=db[DETECTION_COLLECTION_NAME].insert_one(detection)

    print('Created detection record: { videoName:'+videoName+', eventFrame:'+str(eventFrame)+', eventTime:'+str(eventTime)+' }')

def GetVideoFilePath(videoName):
    result=db[VIDEO_COLLECTION_NAME].find_one({'name':videoName})
    
    videoFilePath=""
    if result != None:
        videoFilePath = result["filePath"]
        print("videoFilePath: "+videoFilePath)
    
    return videoFilePath

def HandleVideoStreaming(conn):
    requestedFrameNoBytes = conn.recv(FRAME_NO_BYTE_NUM)
    requestedFrameNo = int.from_bytes(requestedFrameNoBytes, sys.byteorder)

    requestedVideoNameBytes = b""
    buffSize = DEFAULT_READ_BUFFER_SIZE
    while True:
        data = conn.recv(buffSize)
        requestedVideoNameBytes+=data
        if len(data)<buffSize:
            break       

    # requestedVideoName = requestedVideoNameBytes.decode("utf-8")
    requestedVideoName = requestedVideoNameBytes.decode("ascii")

    videoFilePath = GetVideoFilePath(requestedVideoName)
    videoCap = cv2.VideoCapture(videoFilePath)
    
    frameStart = max(0,requestedFrameNo-25)
    frameEnd = requestedFrameNo+25

    frame_count = frameStart

    videoCap.set(cv2.CAP_PROP_POS_FRAMES,frameStart)
    while True:
        ret, frame = videoCap.read()
        #cv2.resize(frame, (640, 360));

        if ret is False:
            print("Video streaming completed!")
            frameBytesLength = (0).to_bytes(FRAME_SIZE_BYTE_NUM, sys.byteorder)
            conn.sendall(frameBytesLength)
            break
        
        # print(frame_count)
        # if frame_count<FRAME_START:
        #     frame_count += 1
        #     continue

        if frame_count>frameEnd:
            frameBytesLength = (0).to_bytes(FRAME_SIZE_BYTE_NUM, sys.byteorder)
            conn.sendall(frameBytesLength)
            break

        pngBytes = cv2.imencode('.png',frame)
        # print(len(pngBytes[1]))
        frameBytesLength = len(pngBytes[1]).to_bytes(FRAME_SIZE_BYTE_NUM, sys.byteorder)
        # frameBytesLength = to_bytes(len(pngBytes[1]),FRAME_SIZE_BYTE_NUM, sys.byteorder)
        # print(sys.byteorder)

        # print(len(frameBytesLength))

        conn.sendall(frameBytesLength)
        conn.sendall(pngBytes[1])
        
        frame_count += 1

        # Display the resulting frame
        # cv2.imshow('frame', frame)

        # if cv2.waitKey(1) & 0xFF == ord('q'):
        #     break

def HandleMessageListRequest(conn):
    detectionDocuments = db[DETECTION_COLLECTION_NAME].find({})
    strToSend = " "
    for document in detectionDocuments:
        strToSend += ("{\"v\":\""+document["videoName"]+"\",\"f\":"+str(document["eventFrame"])+",\"t\":\""+document["eventTime"]+"\"}|")
    
    if strToSend != " ":
        strToSend = strToSend[:-1]
    
    print("strToSend:"+strToSend)
    strBytesToSend = strToSend.encode("ascii")
    strLengthBytes = len(strBytesToSend).to_bytes(MESSAGE_LIST_LENGTH_BYTE_NUM, sys.byteorder)

    conn.sendall(strLengthBytes)
    conn.sendall(strBytesToSend)

def HandleFallDetectionEvent(conn):
    deviceDocuments = db[DEVICE_COLLECTION_NAME].find({})

    for deviceDocument in deviceDocuments:
        threading.Thread(target=SendNotification, args = (deviceDocument["endpointArn"],)).start()
        
    detectionDataBytes = b""
    buffSize = DEFAULT_READ_BUFFER_SIZE
    while True:
        data = conn.recv(buffSize)
        print("data:"+str(data))
        detectionDataBytes+=data
        print("detectionDataBytes:"+str(detectionDataBytes))
        if len(data)<buffSize:
            break       

    print("detectionDataBytes:"+str(detectionDataBytes))
    detectionDataStr = detectionDataBytes.decode("ascii")
    print("detectionDataStr:"+detectionDataStr)
    detectionData = json.loads(detectionDataStr)
    SaveDetection(detectionData["v"],int(detectionData["f"]),detectionData["t"])
    
def HandleVideoFilePathRequest(conn):  
    videoNameBytes = b""
    buffSize = DEFAULT_READ_BUFFER_SIZE
    while True:
        data = conn.recv(buffSize)
        videoNameBytes+=data
        if len(data)<buffSize:
            break       

    videoName = videoNameBytes.decode("ascii")
    print("videoName:"+videoName)
    strToSend = GetVideoFilePath(videoName)
    
    print("strToSend:"+strToSend)
    strBytesToSend = strToSend.encode("ascii")
    conn.sendall(strBytesToSend)

def HandleSubscriptionRequest(conn):
    deviceTokenBytes = b""
    buffSize = DEFAULT_READ_BUFFER_SIZE
    while True:
        data = conn.recv(buffSize)
        deviceTokenBytes+=data
        if len(data)<buffSize:
            break       

    deviceToken = deviceTokenBytes.decode("ascii")
    print("deviceToken:"+deviceToken)
    # client = MongoClient(HOST, PORT)
    # db = client[DB_NAME]

    result = db[DEVICE_COLLECTION_NAME].find_one({'token':deviceToken})

    strToSend = "0"
    if result!=None:
        print('Device with token \'{0}\' already exists'.format(deviceToken)) 
        strToSend = "2"
    else:
        response = snsClient.create_platform_endpoint(
            PlatformApplicationArn=platformApplicationArn,
            Token=deviceToken
        )
        endpointArn = response['EndpointArn']

        device = {
            'token': deviceToken,
            'endpointArn':endpointArn
        }
        result = db[DEVICE_COLLECTION_NAME].insert_one(device)
    
        print('Added device with token \'{0}\' and endpointArn '.format(deviceToken,endpointArn))

        # registratedTokens.append(deviceToken)
        
        # print('EndpointArn: {0}'.format(endpointArn))
        # endpointsArn.append(endpointArn)
        strToSend = "1"

    print("result:"+strToSend)
    strBytesToSend = strToSend.encode("ascii")
    conn.sendall(strBytesToSend)

def HandleConnection(conn):
    commandIDBytes = conn.recv(COMMAND_ID_BYTE_NUM)
    commandID = commandIDBytes.decode("ascii")
    print("commandID: "+str(commandID))

    if commandID == "1001":
        HandleFallDetectionEvent(conn)
    if commandID == "1002":
        HandleVideoFilePathRequest(conn)
    elif commandID == "0001":
        HandleVideoStreaming(conn)
    elif commandID == "0002":
        HandleMessageListRequest(conn)
    elif commandID == "0003":
        HandleSubscriptionRequest(conn)

# Main
snsClient = boto3.client('sns')

response = snsClient.create_platform_application(
    Name=PLATFORM_APPLICATION_NAME,
    Platform='GCM',
    Attributes={
        'PlatformCredential': FCM_SERVER_API_KEY
    }
)

platformApplicationArn = response['PlatformApplicationArn']

mongoClient = MongoClient(DB_HOST, DB_PORT)
db = mongoClient[DB_NAME]

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((SERVER_HOST, SERVER_PORT))
s.listen(5)

threads = []
while(True):
    print("Waiting for connection request...")
    conn, addr = s.accept()
    print("Conencted to " + str(addr))

    t = threading.Thread(target=HandleConnection, args = (conn,))
    threads.append(t)
    t.start()
