#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HONameDetails.h"
#include "FibexTypes.h"
#include "HOCompuScale.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType COMPU-METHODS
 *
 * Container for COMPU-METHOD elements
 */
class ASAM_FIBEX_EXPORT HOCompInternalToPhys
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOCompInternalToPhys(FXFibex *fibex = nullptr);
    ~HOCompInternalToPhys();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element ho:COMPU-SCALES */
    QList<HOCompuScale*> m_compuScales;
    /** @todo element ho:COMPU-DEFAULT-VALUE */
};

} // FIBEX
} // ASAM
