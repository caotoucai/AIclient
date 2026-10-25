#include "FXControllerType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXControllerType::FXControllerType(FXFibex *fibex)
    : FXNamedElementType()
    , m_fibex(fibex)
{
}

void FXControllerType::load(const QDomElement &element)
{
    FXNamedElementType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduTypeCt::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:VENDOR-NAME") {
            m_vendorName = childElement.text();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXPduTypeCt::load, m_vendorName =" << m_vendorName;
#endif
        } else if (childElement.tagName() == "fx:CHIP-NAME") {
            m_chipName = childElement.text();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXPduTypeCt::load, m_chipName =" << m_chipName;
#endif
        } else if (childElement.tagName() == "fx:TERMINATION-IMPEDANCE") {
            m_terminationImpedance = childElement.text().toULong();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXPduTypeCt::load, m_terminationImpedance =" << m_terminationImpedance;
#endif
        } else if (childElement.tagName() == "fx:TRANSCEIVER") {
            m_transceiver = childElement.text();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXPduTypeCt::load, m_transceiver =" << m_transceiver;
#endif
        }

        child = child.nextSibling();
    }


}

} // FIBEX
} // ASAM
