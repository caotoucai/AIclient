#include "FXConnectorType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXConnectorType::FXConnectorType(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXConnectorType::~FXConnectorType()
{
    qDeleteAll(m_inputPorts);
    m_inputPorts.clear();

    qDeleteAll(m_outputPorts);
    m_outputPorts.clear();
}

void FXConnectorType::load(const QDomElement &element)
{
    m_id = element.attribute("ID");

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXConnectorType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:CHANNEL-REF") {
            m_channelRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXConnectorType::load, m_channelRef =" << m_channelRef;
#endif
        } else if (childElement.tagName() == "fx:CONTROLLER-REF") {
            m_controllerRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXConnectorType::load, m_controllerRef =" << m_controllerRef;
#endif
        } else if (childElement.tagName() == "fx:INPUTS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXConnectorType::load INPUTS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:INPUT-PORT") {
                    FXEcuPortType *port = new FXEcuPortType(m_fibex);
                    port->load(subChildElement);
                    m_inputPorts[port->m_id] = port;
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:OUTPUTS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXConnectorType::load OUTPUTS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:OUTPUT-PORT") {
                    FXEcuPortType *port = new FXEcuPortType(m_fibex);
                    port->load(subChildElement);
                    m_outputPorts[port->m_id] = port;
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
