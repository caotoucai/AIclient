#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType COMPU-RATIONAL-COEFFS
 *
 * ASAM rational coefficient.
 */
class ASAM_FIBEX_EXPORT HOCompuRationalCoeffs
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOCompuRationalCoeffs(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element ho:COMPU-NUMERATOR */
    QList<double> m_compuNumeratorList;
    /** element ho:COMPU-DENOMINATOR */
    QList<double> m_compuDenominatorList;
};

} // FIBEX
} // ASAM
