#include "FXActiveConditionType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXActiveConditionType::FXActiveConditionType()
{
}

void FXActiveConditionType::load(const QDomElement &element)
{
    m_systemTriggerList.clear();

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXEventControlledTimingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SYSTEM-TRIGGER") {
            FXSystemTrigger tr;
            tr.load(childElement);
            m_systemTriggerList.append(tr);
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
