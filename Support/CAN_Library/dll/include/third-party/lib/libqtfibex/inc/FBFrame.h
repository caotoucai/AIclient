#pragma once

#include "platform.h"
#include "asam_fibex_export.h"
#include "FibexTypes.h"

#include <QDebug>

namespace ASAM {
namespace FIBEX {

class FXFrameTypeCt;
class FBPdu;
class FBSignal;
class FBEcu;

class ASAM_FIBEX_EXPORT FBFrame
{
public:
    FBFrame();
    ~FBFrame();

    QString name() const;
    quint32 byteLength() const;
    FibexTypes::FXFrameTypeSt frameType() const;

    const FXFrameTypeCt *m_fxFrame = nullptr;
    qint32 m_slotId;
    qint16 m_baseCycle;
    qint16 m_cycleRepetition;
    qint16 m_cycleCounter;
    quint32 m_cycleTime;
    bool m_isTimeTriggered;
    FBEcu *transmitter;
    QHash<QString, FBPdu*> m_pdus;
    QHash<QString, FBSignal*> m_signals;
};

QDebug inline operator<<(QDebug d, const FBFrame &o)
{
    QDebugStateSaver saver(d);
    d.nospace().noquote()  << "FBFrame: ("
      << "name:"    << o.name() << ","
      << "Blen:"    << o.byteLength() << ","
      << "t:"       << (int)o.frameType() << ","
      << "id:"      << o.m_slotId << ","
      << "bc:"      << o.m_baseCycle << ","
      << "crp:"     << o.m_cycleRepetition << ","
      << "cc:"      << o.m_cycleCounter << ","
      << "pdu:"     << o.m_pdus.count() << ","
      << "sig:"     << o.m_signals.count()
      << ")";

    return d;
}

} // FIBEX
} // ASAM
