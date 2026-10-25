#ifndef FBFRAMES_H
#define FBFRAMES_H
#include <QMap>
#include "inc/FibexFile.h"

//struct FBFramesInfo
//{
//    xl_device_t *device;
//    QMap<QString, ASAM::FIBEX::FBFrame*> FBFramesMap;
//};

struct FibexSignalInfo
{
    char name[50];
    char description[200];
    double defaultValue;
    char valueRange[30][100];
    quint8 valueRangeNum;
    double minPhyValue;
    double maxPhyValue;
    char unit[10];
    double factor;
    double offset;
    char transmitter[20];
    char receivers[30][20];
    quint8 receiversNum;
};

struct FibexFrameInfo
{
    qint32 slotId;
    qint16 baseCycle;
    qint16 cycleRepetition;
    qint16 cycleCounter;
    quint32 cycleTime;
    bool isTimeTriggered;
    char name[50];
    char transmitter[20];
    char receivers[30][20];
    quint8 receiverNum;
    FibexSignalInfo signalsList[100];
    quint8 signalsNum;
};

struct FramesInNode
{
    QMap<QString, ASAM::FIBEX::FBFrame*> inputFrames;
    QMap<QString, ASAM::FIBEX::FBFrame*> outputFrames;
};

class FBFrames
{
public:
    FBFrames();
    static QMap<QString, ASAM::FIBEX::FBFrame*> FBFramesInfoMap;
    static QMap<QString, FramesInNode> FBFrmsInNodeMap;   //node name; FramesInNode
    static uint64_t getSignalRawValue(const QString frameName, const QString sigName, const uint8_t *data);
    static double getSignalPhyValue(const QString frameName, const QString sigName, const uint8_t *data);
};

#endif // FBFRAMES_H
