#include "FRConnectorType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FRConnectorType::FRConnectorType(FXFibex *fibex)
    : FXConnectorType(fibex)
    , m_fibex(fibex)
{
}

void FRConnectorType::load(const QDomElement &element)
{
    FXConnectorType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FRConnectorType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "flexray:WAKE-UP-CHANNEL") {
            m_wakeUpChannel = QVariant(childElement.text()).toBool();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRConnectorType::load, m_wakeUpChannel =" << m_wakeUpChannel;
#endif
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
