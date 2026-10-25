#include "FXRelativelyScheduledTimingType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXRelativelyScheduledTimingType::FXRelativelyScheduledTimingType(FXFibex *fibex)
    : m_fibex(fibex)
{
}

void FXRelativelyScheduledTimingType::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXRelativelyScheduledTimingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:DESC") {
            m_desc = childElement.text();
        } else if (childElement.tagName() == "fx:SCHEDULE-TABLE-NAME") {
            m_scheduleTableName = childElement.text();
        } else if (childElement.tagName() == "fx:POSITION-IN-TABLE") {
            m_positionInTable = childElement.text().toShort();
        } else if (childElement.tagName() == "fx:DELAY") {
            m_delay = childElement.text();
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
