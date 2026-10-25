#include "FXSignalGroupType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSignalGroupType::FXSignalGroupType(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
{
}

FXSignalGroupType::~FXSignalGroupType()
{
    if (m_bitLength) delete m_bitLength;
    if (m_identifier) delete m_identifier;
    qDeleteAll(m_orderedSignals);
    m_orderedSignals.clear();
}

void FXSignalGroupType::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXSignalGroupType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:BIT-LENGTH") {
            if (!m_bitLength) {
                m_bitLength = new quint32();
                *m_bitLength = childElement.text().toUInt();
            }
        } else if (childElement.tagName() == "fx:IDENTIFIER") {
            if (!m_identifier) {
                m_identifier = new FXIdentifier();
                m_identifier->load(childElement);
            }
        } else if (childElement.tagName() == "fx:ORDERED-SIGNALS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXRequirements::load ORDERED-SIGNALS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:ORDERED-SIGNAL") {
                    FXOrderedSignal *c = new FXOrderedSignal(m_fibex);
                    c->load(subChildElement);
                    m_orderedSignals.append(c);
                }
                subChild = subChild.nextSibling();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
