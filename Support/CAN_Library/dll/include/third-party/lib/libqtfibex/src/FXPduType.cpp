#include "FXPduType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXPduTypeCt::FXPduTypeCt(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
{
}

FXPduTypeCt::~FXPduTypeCt()
{
    if (m_sigInstances) delete m_sigInstances;
}

void FXPduTypeCt::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduTypeCt::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:BYTE-LENGTH") {
            m_byteLength = childElement.text().toUInt();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXPduTypeCt::load, m_byteLength =" << m_byteLength;
#endif
        } else if (childElement.tagName() == "fx:PDU-TYPE") {
            QString typeStr = childElement.text();
            FibexTypes::EnumParser<FibexTypes::FXPduTypeSt> ep;
            FibexTypes::FXPduTypeSt type;
            bool isOk = ep.str2Enum(typeStr, type);
            if (isOk) {
                m_pduType = type;
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXPduTypeCt::load, m_pduType =" << typeStr << (int)m_pduType;
#endif
            }
        } else if (childElement.tagName() == "fx:SIGNAL-INSTANCES") {
            if (!m_sigInstances) {
                m_sigInstances = new FXSignalInstances(m_fibex);
                m_sigInstances->load(childElement);
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
