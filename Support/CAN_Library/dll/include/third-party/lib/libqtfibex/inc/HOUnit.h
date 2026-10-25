#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HONameDetails.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType UNIT
 *
 * ASAM unit definition
 */
class ASAM_FIBEX_EXPORT HOUnit : public HONameDetails
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOUnit(FXFibex *fibex = Q_NULLPTR);
    ~HOUnit();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** attribute ID */
    QString m_id;
    /** element ho:DISPLAY-NAME */
    QString *m_displayName = nullptr;
    /** element ho:FACTOR-SI-TO-UNIT */
    double *m_factorSiToUnit = nullptr;
    /** element ho:OFFSET-SI-TO-UNIT */
    double *m_offsetSiToUnit = nullptr;
    /** element ho:PHYSICAL-DIMENSION-REF */
    QString *m_physicalDimensionRef = nullptr;
};

} // FIBEX
} // ASAM
