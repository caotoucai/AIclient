#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXCommonLayoutDetails.h"
#include "FXSignalRef.h"

#include <QDebug>

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CODING-TYPE
 *
 * Content model for the entity CODING.
 */
class ASAM_FIBEX_EXPORT FXOrderedSignal
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXOrderedSignal(FXFibex *fibex = nullptr);
    ~FXOrderedSignal();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** group COMMON-LAYOUT-DETAILS */
    FXCommonLayoutDetails m_commLayoutDetails;

    /** @opt element SIGNAL-REF*/
    FXSignalRef m_sigRef;
};

} // FIBEX
} // ASAM
