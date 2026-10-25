#include "FRChannelType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FRChannelType::FRChannelType(FXFibex *fibex)
    : FXChannelType(fibex)
{
}

void FRChannelType::load(const QDomElement &element)
{
    FXChannelType::load(element);

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXEventControlledTimingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "flexray:FLEXRAY-CHANNEL-NAME") {
            m_flexrayChannelName = childElement.text();
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
