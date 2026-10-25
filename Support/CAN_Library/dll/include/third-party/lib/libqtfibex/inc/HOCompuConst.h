#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType/element SIGNAL-REF
 *
 * Refering a signal
 */
class ASAM_FIBEX_EXPORT HOCompuConst
{
public:
    HOCompuConst();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @must element ho:V */
    double m_v;
    /** @must element ho:VT */
    QString m_vt;
};

} // FIBEX
} // ASAM
