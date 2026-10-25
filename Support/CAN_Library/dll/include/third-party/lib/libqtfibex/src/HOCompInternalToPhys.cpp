#include "HOCompInternalToPhys.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompInternalToPhys::HOCompInternalToPhys(FXFibex *fibex)
    : m_fibex(fibex)
{
}

HOCompInternalToPhys::~HOCompInternalToPhys()
{
    qDeleteAll(m_compuScales);
    m_compuScales.clear();
}

void HOCompInternalToPhys::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOCompInternalToPhys::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:COMPU-SCALES") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOCompInternalToPhys::load COMPU-SCALES" << subChildElement.tagName();
#endif
                if (subChildElement.tagName() == "ho:COMPU-SCALE") {
                    HOCompuScale *compuScale = new HOCompuScale(m_fibex);
                    compuScale->load(subChildElement);
                    m_compuScales.append(compuScale);
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "ho:COMPU-DEFAULT-VALUE") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
