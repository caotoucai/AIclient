#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"
#include "HOCompuRationalCoeffs.h"
#include "HOCompuConst.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType COMPU-SCALE
 *
 * Domain and computition rule to convert an internal data (transmitted over
 * the communication system) into phyiscal data of an application.
 *
 */
class ASAM_FIBEX_EXPORT HOCompuScale
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOCompuScale(FXFibex *fibex = nullptr);
    ~HOCompuScale();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element ho:SHORT-LABEL */
    /** @todo element ho:DESC */
    /** @todo element ho:MASK */
    /** @opt element ho:LOWER-LIMIT */
    double *m_lowerLimit = nullptr;
    /** @opt attribute ho:LOWER-LIMIT ho:INTERVAL-TYPE */
    FibexTypes::HOIntervalTypeAb m_lowerLimitIntervalType;
    /** @opt element ho:UPPER-LIMIT */
    double *m_upperLimit = nullptr;
    /** @opt attribute ho:UPPER-LIMIT ho:INTERVAL-TYPE */
    FibexTypes::HOIntervalTypeAb m_upperLimitIntervalType;
    /** @must element ho:COMPU-CONST */
    HOCompuConst m_compuConst;
    /** @must element ho:COMPU-RATIONAL-COEFFS */
    HOCompuRationalCoeffs *m_compuRationalCoeffs = nullptr;
    /** @todo element ho:COMPU-GENERIC-MATH */
};

} // FIBEX
} // ASAM
