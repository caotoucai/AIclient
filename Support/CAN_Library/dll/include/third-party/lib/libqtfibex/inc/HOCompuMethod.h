#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HONameDetails.h"
#include "FibexTypes.h"
#include "HOScaleConstrType.h"
#include "HOCompInternalToPhys.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief element COMPU-METHOD
 *
 * Method to calulate the transmitted value to the physical representation
 */
class ASAM_FIBEX_EXPORT HOCompuMethod : public HONameDetails
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOCompuMethod(FXFibex *fibex = nullptr);
    ~HOCompuMethod();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @required element ho:CATEGORY */
    FibexTypes::HOCompuCategorySt m_compuCategory;
    /** @opt element ho:UNIT-REF */
    QString *m_unitRef = nullptr;
    /** @opt element ho:PHYS-CONSTRS */
    HOScaleConstrType *m_physConstrs = nullptr;
    /** @opt element ho:INTERNAL-CONSTRS */
    HOScaleConstrType *m_internalConstrs = nullptr;
    /** @opt element ho:COMPU-INTERNAL-TO-PHYS */
    HOCompInternalToPhys *m_compuInternalToPhys = nullptr;
    /** @todo element ho:DESC */
};

} // FIBEX
} // ASAM
