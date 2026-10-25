#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType SCALE-CONSTR
 *
 * For reusability scale constraints content model is specified using a type
 * definition.
 *
 */
class ASAM_FIBEX_EXPORT HOScaleConstr
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOScaleConstr(FXFibex *fibex = nullptr);
    ~HOScaleConstr();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @opt element ho:LOWER-LIMIT */
    double *m_lowerLimit = nullptr;
    /** attribute ho:LOWER-LIMIT ho:INTERVAL-TYPE */
    FibexTypes::HOIntervalTypeAb m_lowerLimitIntervalType;
    /** @opt element ho:UPPER-LIMIT */
    double *m_upperLimit = nullptr;
    /** attribute ho:UPPER-LIMIT ho:INTERVAL-TYPE */
    FibexTypes::HOIntervalTypeAb m_upperLimitIntervalType;
};

} // FIBEX
} // ASAM
