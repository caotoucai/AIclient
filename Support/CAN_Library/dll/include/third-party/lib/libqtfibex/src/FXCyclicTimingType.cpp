#include "FXCyclicTimingType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXCyclicTimingType::FXCyclicTimingType()
{
}

void FXCyclicTimingType::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXCyclicTimingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:REPEATING-TIME-RANGE") {
            m_repeatingTimeRange.load(childElement);
        } else if (childElement.tagName() == "fx:STARTING-TIME-RANGE") {
            m_startingTimeRange.load(childElement);
        } else if (childElement.tagName() == "fx:ACTIVE-CONDITION") {
            m_activeCondition.load(childElement);
        } else if (childElement.tagName() == "fx:START-CONDITION") {
            m_startCondition.load(childElement);
        } else if (childElement.tagName() == "fx:STOP-CONDITION") {
            m_stopCondition.load(childElement);
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
