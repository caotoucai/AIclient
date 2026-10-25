#include "fbframes.h"
#include "./froperator.h"

QMap<QString, ASAM::FIBEX::FBFrame*> FBFrames::FBFramesInfoMap;
QMap<QString, FramesInNode> FBFrames::FBFrmsInNodeMap;

FBFrames::FBFrames()
{

}

uint64_t FBFrames::getSignalRawValue(const QString frameName, const QString sigName, const uint8_t *data)
{
    if (!FBFramesInfoMap.contains(frameName)) return -1;
    ASAM::FIBEX::FBSignal *pSig = getSignalFromFrame(FBFramesInfoMap[frameName], sigName.toStdString());
    if (pSig == nullptr) return LLONG_MAX;
    return pSig->decode(data);
}

double FBFrames::getSignalPhyValue(const QString frameName, const QString sigName,const uint8_t *data)
{
    if (!FBFramesInfoMap.contains(frameName)) return -1;
    ASAM::FIBEX::FBSignal *pSig = getSignalFromFrame(FBFramesInfoMap[frameName], sigName.toStdString());
    if (pSig == nullptr) return LLONG_MAX;
    return pSig->decodePhy(data);
}
