#include "FXPduInstances.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

FXPduInstances::FXPduInstances(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXPduInstances::~FXPduInstances()
{
    qDeleteAll(m_pduInstances);
    m_pduInstances.clear();
}

void FXPduInstances::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduInstances::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:PDU-INSTANCE") {
            FXPduInstanceType *pi = new FXPduInstanceType(m_fibex);
            pi->load(childElement);
            m_pduInstances[pi->m_id] = pi;
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
