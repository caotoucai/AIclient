#include "FXRequirements.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXRequirements::FXRequirements(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXRequirements::~FXRequirements()
{
    qDeleteAll(m_signalGroups);
    m_signalGroups.clear();
}


void FXRequirements::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXProcessingInformation::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SIGNAL-GROUPS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXRequirements::load SIGNAL-GROUPS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:SIGNAL-GROUP") {
                    FXSignalGroupType *c = new FXSignalGroupType(m_fibex);
                    c->load(subChildElement);
                    m_signalGroups[c->m_id] = c;
                }
                subChild = subChild.nextSibling();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
