#include "FRWakeUp.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FRWakeUp::FRWakeUp()
{
}

FRWakeUp::~FRWakeUp()
{
    if (m_wakeUpSymbolTxIdle) delete m_wakeUpSymbolTxIdle;
    if (m_wakeUpSymbolTxLow) delete m_wakeUpSymbolTxLow;
}

void FRWakeUp::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduTypeCt::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "flexray:WAKE-UP-SYMBOL-RX-IDLE") {
            m_wakeUpSymbolRxIdle = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:WAKE-UP-SYMBOL-RX-LOW") {
            m_wakeUpSymbolRxLow = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:WAKE-UP-SYMBOL-RX-WINDOW") {
            m_wakeUpSymbolRxWindow = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:WAKE-UP-SYMBOL-TX-IDLE") {
            if (!m_wakeUpSymbolTxIdle) {
                m_wakeUpSymbolTxIdle = new quint16();
                *m_wakeUpSymbolTxIdle = childElement.text().toUShort();
            }
        } else if (childElement.tagName() == "flexray:WAKE-UP-SYMBOL-TX-LOW") {
            if (!m_wakeUpSymbolTxLow) {
                m_wakeUpSymbolTxLow = new quint16();
                *m_wakeUpSymbolTxLow = childElement.text().toUShort();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
