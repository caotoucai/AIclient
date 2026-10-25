#include "HOCompuMethods.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompuMethods::HOCompuMethods(FXFibex *fibex)
    : m_fibex(fibex)
{
}

HOCompuMethods::~HOCompuMethods()
{
    qDeleteAll(m_compuMethods);
    m_compuMethods.clear();
}

void HOCompuMethods::load(const QDomElement &element)
{
    m_compuMethods.clear();

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOCompuMethods::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:COMPU-METHOD") {
            HOCompuMethod *cm = new HOCompuMethod(m_fibex);
            cm->load(childElement);
            m_compuMethods.append(cm);
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
