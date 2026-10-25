#include "FXSignalInstances.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSignalInstances::FXSignalInstances(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXSignalInstances::~FXSignalInstances()
{
    qDeleteAll(m_sigInstances);
    m_sigInstances.clear();
}

void FXSignalInstances::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXSignalInstances::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SIGNAL-INSTANCE") {
            FXSignalInstanceType *si = new FXSignalInstanceType(m_fibex);
            si->load(childElement);
            m_sigInstances[si->m_id] = si;
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
