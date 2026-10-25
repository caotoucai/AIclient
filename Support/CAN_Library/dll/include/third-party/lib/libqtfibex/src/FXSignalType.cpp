#include "../inc/FXSignalType.h"
#include "../inc/LogDb.h"
#include "../inc/FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSignalType::FXSignalType(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
    , m_codingRef()
{
}

FXSignalType::~FXSignalType()
{
    if (m_defaultValue) delete m_defaultValue;
    if (m_sigType) delete m_sigType;
    if (m_priority) delete m_priority;
}

void FXSignalType::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXSignalType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:DEFAULT-VALUE") {
            if (!m_defaultValue) {
                m_defaultValue = new double();
                *m_defaultValue = childElement.text().toDouble();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXSignalType::load, m_defaultValue =" << "sig:" << m_shortName << " "<< *m_defaultValue;
#endif
            }

        } else if (childElement.tagName() == "fx:CODING-REF") {
            m_codingRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXSignalType::load, m_codingRef =" << m_codingRef;
#endif
        } else if (childElement.tagName() == "fx:SIGNAL-TYPE") {
            if (!m_sigType) {
                m_sigType = new FXSignalTypeType();
                m_sigType->load(childElement);
            }
        } else if (childElement.tagName() == "fx:PRIORITY") {
            if (!m_priority) {
                m_priority = new quint32();
                *m_priority = childElement.text().toUInt();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
