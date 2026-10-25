#include "FXRequestControlledTimingType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXRequestControlledTimingType::FXRequestControlledTimingType()
{
}

void FXRequestControlledTimingType::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXRequestControlledTimingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:RESPONSE-TIME-RANGE") {
            m_responseTimeRange.load(childElement);
        } else if (childElement.tagName() == "fx:ACTIVE-CONDITION") {
            m_activeCondition.load(childElement);
        } else if (childElement.tagName() == "fx:FINAL-REPETITIONS") {
            m_finalRepetitions = childElement.text().toUShort();
        } else if (childElement.tagName() == "ho:DESC") {
            m_desc = childElement.text();
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
