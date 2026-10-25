#include "FXSystemTrigger.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSystemTrigger::FXSystemTrigger()
{
}

void FXSystemTrigger::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXSystemTrigger::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SYSTEM-STATE") {
            QString typeStr = childElement.text();
            FibexTypes::EnumParser<FibexTypes::FXActiveConditionSystemStateType> ep;
            FibexTypes::FXActiveConditionSystemStateType type;
            bool isOk = ep.str2Enum(typeStr, type);
            if (isOk) {
                m_systemStateList.append(type);
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXSystemTrigger::load, m_systemStateList append" << typeStr << (int)type;
#endif
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
