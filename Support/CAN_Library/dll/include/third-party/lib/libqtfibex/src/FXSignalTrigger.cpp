#include "FXSignalTrigger.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSignalTrigger::FXSignalTrigger()
    : FXRevisedElementType()
{
}

void FXSignalTrigger::load(const QDomElement &element)
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
            FibexTypes::EnumParser<FibexTypes::FXExecuteConditionSignalStateType> ep;
            FibexTypes::FXExecuteConditionSignalStateType type;
            bool isOk = ep.str2Enum(typeStr, type);
            if (isOk) {
                m_signalStateList.append(type);
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXSignalTrigger::load, m_signalStateList append" << typeStr << (int)type;
#endif
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM

