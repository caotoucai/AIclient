#include "HOCompuConst.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompuConst::HOCompuConst()
{
}

void HOCompuConst::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOCompuConst::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:V") {
            m_v = childElement.text().toDouble();
        } else if (childElement.tagName() == "ho:VT") {
            m_vt = childElement.text();
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
