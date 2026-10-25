#include "FXOrderedSignal.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXOrderedSignal::FXOrderedSignal(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXOrderedSignal::~FXOrderedSignal()
{

}

void FXOrderedSignal::load(const QDomElement &element)
{
    m_commLayoutDetails.load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXOrderedSignal::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SIGNAL-REF") {
            m_sigRef.load(childElement);
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
