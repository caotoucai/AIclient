#include "FXCodings.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXCodings::FXCodings(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXCodings::~FXCodings()
{
    qDeleteAll(m_codings);
    m_codings.clear();
}

void FXCodings::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXCodings::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:CODING") {
            CodingType *ct = new CodingType(m_fibex);
            ct->load(childElement);
            m_codings[ct->m_id] = ct;
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
