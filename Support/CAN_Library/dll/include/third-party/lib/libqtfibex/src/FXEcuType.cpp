#include "FXEcuType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXEcuType::FXEcuType(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
{
}

FXEcuType::~FXEcuType()
{
    qDeleteAll(m_controllers);
    m_controllers.clear();

    qDeleteAll(m_connectors);
    m_connectors.clear();
}

void FXEcuType::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXEcuType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:DIAGNOSTIC-ADDRESSES") {
#ifndef F_NO_DEBUG
            //QLOG_TRACE() << "FXEcuType::load, m_byteLength =" << m_byteLength;
#endif
        } else if (childElement.tagName() == "fx:FUNCTION-REFS") {

        } else if (childElement.tagName() == "fx:CONTROLLERS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXEcuType::load CONTROLLERS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:CONTROLLER") {
                    FRControllerType *c = new FRControllerType(m_fibex);
                    c->load(subChildElement);
                    m_controllers[c->m_id] = c;
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:CONNECTORS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXEcuType::load CONNECTORS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:CONNECTOR") {
                    FRConnectorType *c = new FRConnectorType(m_fibex);
                    c->load(subChildElement);
                    m_connectors[c->m_id] = c;
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
