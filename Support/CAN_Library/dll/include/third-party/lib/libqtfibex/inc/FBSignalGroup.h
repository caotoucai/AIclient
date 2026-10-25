#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

#include <QDebug>

namespace ASAM {
namespace FIBEX {

class FXSignalGroupType;
class FBSignal;
class FBPdu;

class ASAM_FIBEX_EXPORT FBSignalGroup
{
public:
    FBSignalGroup();
    ~FBSignalGroup();

    QString name() const;

    const FXSignalGroupType *m_fxSignalGroup = nullptr;
    quint32 m_bitLength;
    quint32 *m_updateBitPosition = nullptr;

    QHash<QString, FBSignal*> m_signals;
    FBPdu *m_fbPdu = nullptr;
};

QDebug inline operator<<(QDebug d, const FBSignalGroup &o)
{
    QDebugStateSaver saver(d);
    d.nospace().noquote() << "FBSignalGroup: ("
      << "name:"    << o.name() << ","
      << "blen:"    << o.m_bitLength << ","
      << "ub:"      << (o.m_updateBitPosition?*o.m_updateBitPosition:0xFFFFFFFF) << ","
      << ")";

    return d;
}

} // FIBEX
} // ASAM
