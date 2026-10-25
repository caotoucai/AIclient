#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HOUnitSpec.h"
#include "FXCodings.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType fx:PROCESSING-INFORMATION
 *
 * Data needed to process data in elements.
 */
class ASAM_FIBEX_EXPORT FXProcessingInformation
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXProcessingInformation(FXFibex *fibex = Q_NULLPTR);
    ~FXProcessingInformation();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element ho:UNIT-SPEC */
    HOUnitSpec *m_unitSpec = nullptr;
    /** element fx:CODINGS */
    FXCodings *m_codings = nullptr;
};

} // FIBEX
} // ASAM
