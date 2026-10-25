#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

#include <QDebug>

namespace ASAM {
namespace FIBEX {

class FXPduTypeCt;
class FBSignal;
class FBFrame;

class ASAM_FIBEX_EXPORT FBPdu
{
public:
    FBPdu();
    ~FBPdu();

    QString name() const;
    quint32 byteLength() const;
    FibexTypes::FXPduTypeSt pduType() const;

    const FXPduTypeCt *m_fxPdu = nullptr;
    quint16 m_startBit;
    bool m_isBigEndian;

    QHash<QString, FBSignal*> m_signals;
    FBFrame *m_fbFrame = nullptr;
};

QDebug inline operator<<(QDebug d, const FBPdu &o)
{
    QDebugStateSaver saver(d);
    d.nospace().noquote() << "FBPdu: ("
      << "name:"    << o.name() << ","
      << "Blen:"    << o.byteLength() << ","
      << "t:"       << (int)o.pduType() << ","
      << "sb:"      << o.m_startBit << ","
      << "be:"      << o.m_isBigEndian
      << ")";

    return d;
}

} // FIBEX
} // ASAM
