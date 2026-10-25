#include "FRKeySlotUsage.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FRKeySlotUsage::FRKeySlotUsage()
{
}

void FRKeySlotUsage::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduTypeCt::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "flexray:STARTUP-SYNC") {
            m_keySlotUsage = FibexTypes::FRKeySlotUsageSt::STARTUP_SYNC;
            m_keySlotId = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRKeySlotUsage::load, m_keySlotUsage =" << (int)m_keySlotUsage;
            QLOG_TRACE() << "FRKeySlotUsage::load, m_keySlotId =" << m_keySlotId;
#endif
        } else if (childElement.tagName() == "flexray:SYNC") {
            m_keySlotUsage = FibexTypes::FRKeySlotUsageSt::SYNC;
            m_keySlotId = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRKeySlotUsage::load, m_keySlotUsage =" << (int)m_keySlotUsage;
            QLOG_TRACE() << "FRKeySlotUsage::load, m_keySlotId =" << m_keySlotId;
#endif
        } else if (childElement.tagName() == "flexray:NONE") {
            m_keySlotUsage = FibexTypes::FRKeySlotUsageSt::NONE;
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRKeySlotUsage::load, m_keySlotUsage =" << (int)m_keySlotUsage;
#endif
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
