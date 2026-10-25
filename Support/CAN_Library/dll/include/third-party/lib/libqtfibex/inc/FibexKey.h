#ifndef FIBEXKEY_H
#define FIBEXKEY_H

#include <QtMath>
#include <QHash>
#include <QPair>
#include <QMetaType>
#include <QDebug>

class FibexFrameKey
{
public:
    FibexFrameKey()
        : m_slotId(0), m_baseCycle(0), m_cycleRepetition(1)
    {}

    FibexFrameKey(qint32 slotId, qint16 baseCycle, qint16 cycleRepetition)
        : m_slotId(slotId)
        , m_baseCycle(baseCycle)
        , m_cycleRepetition(cycleRepetition)
    {}

    bool IsInitialValue() {
        if(m_slotId == 0 && m_baseCycle == 0 && m_cycleRepetition == 1) {
            return true;
        }
        return false;
    }
    friend bool operator==(const FibexFrameKey &e1, const FibexFrameKey &e2);
    friend quint32 qHash(const FibexFrameKey &key);

    qint32 m_slotId;
    qint16 m_baseCycle;
    qint16 m_cycleRepetition;
};
Q_DECLARE_METATYPE(FibexFrameKey)

inline bool operator==(const FibexFrameKey &e1, const FibexFrameKey &e2)
{
    return (e1.m_slotId == e2.m_slotId)
            && (e1.m_baseCycle == e2.m_baseCycle)
            && (e1.m_cycleRepetition == e2.m_cycleRepetition);
}

inline bool operator<(const FibexFrameKey &e1, const FibexFrameKey &e2)
{
    return (e1.m_slotId < e2.m_slotId);
}

inline quint32 qHash(const FibexFrameKey &key)
{
    qint64 v = (static_cast<qint64>(key.m_slotId) << 32)
            | (static_cast<qint64>(key.m_baseCycle) << 16)
            | key.m_cycleRepetition;
    return qHash(v);
}

QDebug inline operator<<(QDebug d, const FibexFrameKey &o)
{
    QDebugStateSaver saver(d);
    d.nospace().noquote() << "FibexFrameKey: ("
      << "id:" << o.m_slotId << ","
      << "id:" << o.m_baseCycle << ","
      << "id:" << o.m_cycleRepetition << ","
      << ")";

    return d;
}

#endif // FIBEXKEY_H
